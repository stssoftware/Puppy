using Newtonsoft.Json.Linq;
using Puppy.Elastic.ContextSearch.SearchModel.AggModel.Buckets;
using System.Collections.Generic;

namespace Puppy.Elastic.ContextSearch.SearchModel.AggModel
{
    public class RangesBucketAggregationsResult : AggregationResult<RangesBucketAggregationsResult>
    {
        public List<RangeBucket> Buckets { get; set; }

        public override RangesBucketAggregationsResult GetValueFromJToken(JToken result)
        {
            return result.ToObject<RangesBucketAggregationsResult>();
        }
    }
}