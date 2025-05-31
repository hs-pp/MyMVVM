using System;
using System.Collections.Generic;
using MyMVVM;

[Serializable]
public class BaseModel
{
    public BaseModel()
    {
        AutoGenInit();
    }
    
    public virtual event Action<BaseModel> OnChanged;
    protected virtual void AutoGenInit() { }
}

public class Model : BaseModel
{
    
}

public class ViewModel : BaseModel
{
    
}

public class ObservableAttribute : Attribute
{
    public ObservableAttribute(string nameOverride = "") { }
}

public class PassthroughAttribute : Attribute
{
    public PassthroughAttribute(string modelName, string modelPropertyName, string nameOverride = "") { }
}

public partial class TestModel : Model
{
    [Observable]
    private string m_testString;

    [Observable]
    private List<int> m_testStringList;

    [Observable]
    private AnotherModel m_anotherModel;

    [Observable]
    private List<AnotherModel> m_manyOtherModels;

    private AnotherModel m_passthroughModel;
    
    [Passthrough("m_passthroughModel", "m_anotherString")]
    private string m_anotherString;
    
    [Passthrough("m_passthroughModel", "m_anotherStringList")]
    private List<string> m_anotherStringList;
}

namespace MyMVVM
{
    public partial class AnotherModel : Model
    {
        [Observable]
        private string m_anotherString;

        [Observable]
        private List<string> m_anotherStringList;
    }
}