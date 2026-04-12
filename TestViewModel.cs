using System.Collections.Generic;
using UnityEngine;

namespace MyMVVM
{
    public partial class TestViewModel : ViewModel
    {
        private TestModel m_testModel;
        
        [Observable]
        private string m_testString = "Default String";
        
        [Observable]
        private int m_testInt = 1;
        
        [Observable]
        private float m_testFloat = 1.5f;
        
        [Observable]
        private SecondTestViewModel m_secondTest;
        
        [Passthrough("m_secondTest", "m_testBool")]
        private bool m_testBool = true;
        
        [Passthrough("m_secondTest", "m_coolString")]
        private string m_coolString;

        [Observable]
        private List<SecondTestViewModel> m_testList;
        
        [Observable]
        private Color m_testColor = Color.red;

        [Observable]
        private GameObject m_weirdObject;
    }

    public partial class SecondTestViewModel : ViewModel
    {
        [Observable]
        private bool m_testBool = true;
        
        [Observable]
        private string m_coolString = "truuu";
    }

    public partial class TestModel : Model
    {
        [Observable]
        private string m_testModelString = "Model String";
    }
}
