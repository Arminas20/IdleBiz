using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleBiz.Business
{
    [Serializable]
    public class BusinessDef
    {
        public string Id;
        public string Name;
        public double UnlockCost;
        public bool UnlockedByDefault;
    }

    [CreateAssetMenu(fileName = "BusinessCatalog", menuName = "IdleBiz/Business Catalog")]
    public class BusinessCatalog : ScriptableObject
    {
        public List<BusinessDef> Items = new(); // rodomas sàraðo orderis = èia esanti tvarka
    }
}

