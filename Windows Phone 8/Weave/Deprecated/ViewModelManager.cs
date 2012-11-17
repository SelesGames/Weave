//using System;
//using System.Collections.Generic;

//namespace weave
//{
//    public class ViewModelManager
//    {
//        static Dictionary<string, object> lookup;

//        static ViewModelManager()
//        {
//            lookup = AppSettings.TombstoneState.ViewModelManager;
//            if (lookup == null)
//            {
//                lookup = new Dictionary<string, object>();
//                AppSettings.TombstoneState.ViewModelManager = lookup;
//            }
//        }

//        public static T Get<T>()
//        {
//            var key = typeof(T).AssemblyQualifiedName;
//            if (lookup.ContainsKey(key))
//                return (T)lookup[key];
//            else
//                return default(T);
//        }

//        public static void Set<T>(T obj)
//        {
//            var key = typeof(T).AssemblyQualifiedName;
//            if (lookup.ContainsKey(key))
//                lookup.Remove(key);

//            lookup.Add(key, obj);
//        }
//    }
//}
