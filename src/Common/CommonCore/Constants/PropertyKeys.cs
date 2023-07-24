using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllSub.CommonCore.Constants
{
    public static class PropertyKeys
    {
        public const string Key = nameof(PropertyKeys);

        public static class Vk
        {
            public const string Key = nameof(Vk);
            public const string AccessTokenKey = PropertyKeys.Key + ":" + Vk.Key + ":" + nameof(AccessTokenKey);
            public const string UseerIdKey = PropertyKeys.Key + ":" + Vk.Key + ":" + nameof(UseerIdKey);
        }
        
        public static class Google
        {
            public const string Key = nameof(Google);
            public const string AccessTokenKey = PropertyKeys.Key + ":" + Google.Key + ":" + nameof(AccessTokenKey);
            public const string UseerIdKey = PropertyKeys.Key + ":" + Google.Key + ":" + nameof(UseerIdKey);
        }
    }
}
