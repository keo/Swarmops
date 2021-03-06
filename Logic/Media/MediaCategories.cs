using System.Collections.Generic;
using Swarmops.Basic.Types;
using Swarmops.Database;
using Swarmops.Logic.Support;

namespace Swarmops.Logic.Media
{
    public class MediaCategories : List<MediaCategory>
    {
        public int[] Identities
        {
            get { return LogicServices.ObjectsToIdentifiers (ToArray()); }
        }

        public static MediaCategories FromArray (BasicMediaCategory[] basicArray)
        {
            MediaCategories result = new MediaCategories();

            result.Capacity = basicArray.Length*11/10;
            foreach (BasicMediaCategory basic in basicArray)
            {
                result.Add (MediaCategory.FromBasic (basic));
            }

            return result;
        }

        public static MediaCategories FromIdentities (int[] identities)
        {
            return FromArray (SwarmDb.GetDatabaseForReading().GetMediaCategories (identities));
        }

        public static MediaCategories GetAll()
        {
            return FromArray (SwarmDb.GetDatabaseForReading().GetMediaCategories());
        }
    }
}