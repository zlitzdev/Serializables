using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine.UIElements;

namespace Zlitz.General.Serializables
{
    public class TypeSelector : PopupField<Type>
    {
        private Func<Type, bool> m_filter;

        public Func<Type, bool> filter
        {
            get => m_filter;
            set
            {
                m_filter = value;
                UpdateChoices();
            }
        }

        public TypeSelector(string label = null) : base(label)
        {
            formatListItemCallback      = FormatTypeInList;
            formatSelectedValueCallback = FormatTypeSelected;
        }

        private void UpdateChoices()
        {
            List<Type> newChoices = TypeHelper.types.Where(t => filter?.Invoke(t) ?? true).ToList();
            newChoices.Insert(0, null);
            choices = newChoices;
        }

        private static string FormatTypeInList(Type type)
        {
            if (type == null)
            {
                return "None";
            }

            if (string.IsNullOrEmpty(type.Namespace))
            {
                return $"{type.Name} [{type.Assembly?.GetName().Name ?? "Unknown assembly"}]";
            }

            return $"{type.Namespace}.{type.Name} [{type.Assembly?.GetName().Name ?? "Unknown assembly"}]";
        }

        private static string FormatTypeSelected(Type type)
        {
            if (type == null)
            {
                return "None";
            }

            if (string.IsNullOrEmpty(type.Namespace))
            {
                return $"{type.Name}";
            }

            return $"{type.Namespace}.{type.Name}";
        }
    }

    public static class TypeHelper
    {
        private static List<Type> s_types;

        public static IEnumerable<Type> types
        {
            get
            {
                if (s_types == null)
                {
                    s_types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).ToList();
                }
                return s_types;
            }
        }
    }
}
