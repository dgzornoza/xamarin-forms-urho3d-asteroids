using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace Asteroids.Helpers
{
    /// <summary>
    /// Helpers for manage xaml visual tree
    /// </summary>
    public static class VisualTree
    {

        /// <summary>
        /// Find the page to which an element belongs
        /// </summary>
        /// <returns>The page</returns>
        /// <param name="element">Element</param>
        public static Page FindParentPage(this VisualElement element)
        {
            if (element != null)
            {
                var parent = element.Parent;
                while (parent != null)
                {
                    if (parent is Page)
                    {
                        return parent as Page;
                    }
                    parent = parent.Parent;
                }
            }
            return null;
        }

        /// <summary>
        /// recursive function for search visual child elements in parent visual tree
        /// </summary>
        /// <typeparam name="T">Child visual element type to search</typeparam>
        /// <param name="parentElement">Parent visual element</param>
        /// <param name="whereSearch">element where search</param>
        /// <param name="containsStringName">(optional) Child element name</param>
        /// <param name="result">(optional) Elements list where add result</param>
        /// <returns>Child elements list found</returns>
        public static List<T> FindVisualChildren<T>(this VisualElement parentElement, VisualElement whereSearch, string containsStringName = null, List<T> result = null)
        {
            result = result ?? new List<T>();

            try
            {
                var props = whereSearch.GetType().GetRuntimeProperties();
                var contentProp = props.FirstOrDefault(w => w.Name == "Content");
                var childProp = props.FirstOrDefault(w => w.Name == "Children");
                var itemsProp = props.FirstOrDefault(w => w.Name == "TemplatedItems");
                if (childProp == null) childProp = itemsProp;

                // parent is container
                if (childProp == null && contentProp != null && contentProp.GetValue(whereSearch) is VisualElement cv)
                {
                    FindVisualChildren<T>(parentElement, cv, containsStringName, result);
                    return result;
                }
                
                // parent is not container
                IEnumerable values = childProp.GetValue(whereSearch) as IEnumerable;
                foreach (var value in values)
                {
                    var tempValue = value;
                    if (tempValue is ViewCell) tempValue = ((ViewCell)tempValue).View;
                    if (tempValue is VisualElement) FindVisualChildren<T>(parentElement, tempValue as VisualElement, containsStringName, result);

                    if (tempValue is T)
                    {
                        if (!string.IsNullOrEmpty(containsStringName))
                        {
                            bool check = false;
                            var fields = parentElement.GetType().GetRuntimeFields().Where(w => w.Name.ToLower().Contains(containsStringName.ToLower())).ToList();
                            foreach (var field in fields)
                            {
                                var fieldValue = field.GetValue(parentElement);
                                if (fieldValue is T && fieldValue == tempValue) { check = true; break; }
                            }
                            if (!check) continue;
                        }

                        result.Insert(0, (T)tempValue);
                    }
                }
                return result;
            }
            catch
            {
                return result;
            }
        }


    }
}
