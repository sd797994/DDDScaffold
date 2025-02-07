using System.Collections.Generic;
using System.Linq;

namespace Domain.DomainBase
{
    public class SortedParams
    {
        public bool Sort { get; set; }
        public string SortName { get; set; }
        public static List<SortedParams> BuildSort(params (bool? sort, string sortName)[] inputs)
        {
            var result = new List<SortedParams>();
            if (inputs != null && inputs.Any(x => x.sort != null && x.sortName != null))
                foreach (var input in inputs.Where(x => x.sort != null && x.sortName != null))
                    result.Add(new SortedParams() { Sort = input.sort.Value, SortName = input.sortName });
            return result;
        }
    }
}
