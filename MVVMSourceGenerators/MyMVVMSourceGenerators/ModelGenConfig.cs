using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeInfo = Microsoft.CodeAnalysis.TypeInfo;

namespace MyMVVMSourceGenerators;

public class ModelGenConfig
{
    private static string MODEL_CLASS_NAME = "Model";
    private static string VIEWMODEL_CLASS_NAME = "ViewModel";
    private static string OBSERVABLE_ATTRIBUTE_NAME = "Observable";
    private static string PASSTHROUGH_ATTRIBUTE_NAME = "Passthrough";
    
    public string Namespace { get; private set; }
    public string ClassName { get; private set; }
    public string ParentClassName { get; private set; }
    public bool InheritsModel { get; private set; }
    public bool InheritsViewModel { get; private set; }
    public List<AttributeConfig> RelevantAttributes = new();

    public bool ShouldGen { get; private set; }

    public ModelGenConfig(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDecl)
        {
            return;
        }

        Namespace = GetNamespace(classDecl);
        if (Namespace == "<global namespace>")
        {
            Namespace = string.Empty;
        }
        
        ClassName = classDecl.Identifier.Text;
        ParentClassName = GetParentClass(context.SemanticModel, classDecl);
        if (ParentClassName == MODEL_CLASS_NAME)
        {
            InheritsModel = true;
        }
        if (ParentClassName == VIEWMODEL_CLASS_NAME)
        {
            InheritsViewModel = true;
        }
        if (!InheritsModel && !InheritsViewModel)
        {
            return;
        }
        
        foreach (FieldDeclarationSyntax field in classDecl.Members.OfType<FieldDeclarationSyntax>())
        {
            var attributes = field.AttributeLists.SelectMany(attrList => attrList.Attributes).ToList();
            foreach (AttributeSyntax attr in attributes)
            {
                if (attr.Name.ToString() == OBSERVABLE_ATTRIBUTE_NAME)
                {
                    RelevantAttributes.Add(new ObservableAttributeConfig(field, attr, context.SemanticModel));
                }
                else if (attr.Name.ToString() == PASSTHROUGH_ATTRIBUTE_NAME)
                {
                    RelevantAttributes.Add(new PassthroughAttributeConfig(field, attr, context.SemanticModel));
                }
            }
        }

        if (RelevantAttributes.Count == 0)
        {
             return;
        }
        
        ShouldGen = true;
    }
    
    private string GetNamespace(ClassDeclarationSyntax classDeclaration)
    {
        SyntaxNode? parent = classDeclaration.Parent;
        while (parent != null && parent is not NamespaceDeclarationSyntax)
        {
            parent = parent.Parent;
        }

        if (parent is NamespaceDeclarationSyntax namespaceDeclaration)
        {
            return namespaceDeclaration.Name.ToString();
        }

        return string.Empty; // Return an empty string if no namespace is found
    }

    private string GetParentClass(SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration)
    {
        INamedTypeSymbol iSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        INamedTypeSymbol symbolBaseType = iSymbol?.BaseType;
        return (symbolBaseType == null || symbolBaseType.Name == "Object") ? string.Empty : symbolBaseType.Name;
    }
}

public class AttributeConfig
{
    public string FieldName { get; protected set; }
    public string FieldType { get; protected set; }
    public bool FieldIsTypeModel { get; protected set; }
    public string PropName { get; protected set; }
    public string TypeNamespace { get; protected set; }
    public bool IsList { get; protected set; }
    public string ListType { get; protected set; }
    
    public AttributeConfig(FieldDeclarationSyntax field, AttributeSyntax attr, SemanticModel semanticModel)
    {
        VariableDeclaratorSyntax fieldInfo = field.Declaration.Variables.First();
        FieldName = fieldInfo.Identifier.ToString();
        FieldType = field.Declaration.Type.ToString();
        PropName = ConvertToPropertyName(FieldName);

        TypeInfo typeInfo = semanticModel.GetTypeInfo(field.Declaration.Type);
        INamedTypeSymbol namedTypeSymbol = typeInfo.Type as INamedTypeSymbol;
        ITypeSymbol typeToCheckIfModel = typeInfo.Type;
        
        IsList = namedTypeSymbol != null &&
                 namedTypeSymbol.IsGenericType &&
                 namedTypeSymbol.ConstructedFrom.ToString() == "System.Collections.Generic.List<T>";
        if (IsList)
        {
            ListType = namedTypeSymbol.TypeArguments.First().ToString();
            typeToCheckIfModel = namedTypeSymbol.TypeArguments.First();
        }

        while (typeToCheckIfModel != null)
        {
            if (typeToCheckIfModel.Name == "BaseDataModel")
            {
                FieldIsTypeModel = true;
                break;
            }
            typeToCheckIfModel = typeToCheckIfModel.BaseType;
        }
        
        TypeNamespace = typeInfo.Type.ContainingNamespace.ToString();
    }
    
    protected string ConvertToPropertyName(string fieldName)
    {
        if (fieldName.StartsWith("m_"))
        {
            fieldName = fieldName.Substring(2);
        }
        if (fieldName.StartsWith("_"))
        {
            fieldName = fieldName.Substring(1);
        }
        string propName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
        if (propName == fieldName)
        {
            propName = "_" + propName;
        }

        return propName;
    }
}

public class ObservableAttributeConfig : AttributeConfig
{
    public ObservableAttributeConfig(FieldDeclarationSyntax field, AttributeSyntax attr, SemanticModel semanticModel) 
        : base(field, attr, semanticModel)
    {
        if (attr.ArgumentList != null && attr.ArgumentList.Arguments.Count > 0)
        {
            string nameOverride = attr.ArgumentList.Arguments[0].ToString().Replace("\"", "");
            if (!string.IsNullOrEmpty(nameOverride))
            {
                FieldName = nameOverride;
            }
        }
    }
}

public class PassthroughAttributeConfig : AttributeConfig
{
    public string ModelName { get; private set; }
    public string ModelPropertyName { get; private set; }
    
    public PassthroughAttributeConfig(FieldDeclarationSyntax field, AttributeSyntax attr, SemanticModel semanticModel) 
        : base(field, attr, semanticModel)
    {
        var arguments = attr.ArgumentList.Arguments;
        if (arguments.Count > 2)
        {
            string nameOverride = arguments[2].ToString().Replace("\"", "");
            if (!string.IsNullOrEmpty(nameOverride))
            {
                FieldName = nameOverride;
            }
        }
        ModelName = arguments[0].ToString().Replace("\"", "");
        ModelPropertyName = ConvertToPropertyName(arguments[1].ToString().Replace("\"", ""));
    }
}