using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public abstract class ABaseFieldDrawer : VisualElement
    {
        public abstract Type GetFieldDrawerType();
        public abstract void ToggleMainFoldout(bool toggled);
        public abstract void SetField(IFieldProvider fieldProvider);
        public abstract void ClearDrawer();
        
        public static Dictionary<Type, Type> m_fieldDrawerTypeLookup = new()
        {
            [typeof(string)] = typeof(StringFieldDrawer),
            [typeof(int)] = typeof(IntFieldDrawer),
            [typeof(float)] = typeof(FloatFieldDrawer),
            [typeof(bool)] = typeof(BoolFieldDrawer),
            [typeof(BaseModel)] = typeof(ModelFieldDrawer),
        };
        
        public static ABaseFieldDrawer CreateFieldDrawerForType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return Activator.CreateInstance(typeof(ListFieldDrawer<>).MakeGenericType(type.GetGenericArguments()[0]))
                    as ABaseFieldDrawer;
            }

            Type typeLoop = type;
            while (typeLoop != null)
            {
                if (m_fieldDrawerTypeLookup.TryGetValue(typeLoop, out var value))
                {
                    return Activator.CreateInstance(value) as ABaseFieldDrawer;
                }
                typeLoop = typeLoop.BaseType;
            }

            // Default to fallback.
            return Activator.CreateInstance(typeof(FallbackFieldDrawer<>).MakeGenericType(type)) as ABaseFieldDrawer;
        }
    }

}