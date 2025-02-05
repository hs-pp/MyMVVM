using System;
using System.Collections.Generic;

[Serializable]
public class BaseDataModel
{
    public BaseDataModel()
    {
        AutoGenInit();
    }
    public Action<BaseDataModel> OnChanged;

    protected virtual void AutoGenInit() { }
}

public class Model : BaseDataModel
{
    
}

public class ViewModel : BaseDataModel
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
    private string m_hello;

    [Observable]
    private List<int> stringsss;

    [Observable]
    private AnotherModel m_anotherModel;

    [Observable]
    private List<AnotherModel> m_manyOtherModels;
    
    [Observable]
    private List<AnotherModel> m_huhstrange;
}

public partial class AnotherModel : Model
{
    [Observable]
    private string m_cOOOOOOLString;
}

namespace MyMVVM
{
    public partial class TestViewModel : ViewModel
    {
        [Passthrough("m_testModel", "m_hello")]
        private string m_hey;

        [Passthrough("m_testModel", "stringsss")]
        private List<int> moreStrings;
        
        private TestModel m_testModel;
    }
}