using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using static nunit.xamarin.Extensions.ListExtensions;

namespace nunit.xamarin.Helpers
{
    public class PartitionTestFilter : TestFilter
    {
        private readonly IList<string> _testIdPartition;

        public PartitionTestFilter(HashSet<string> testNames, int partitionIndex, int totalPartitionCount, int seed)
        {
            var allTestNames = testNames.ToList();
            var partitionSize = (int) Math.Ceiling((double) allTestNames.Count / totalPartitionCount);

            allTestNames.Shuffle(new Random(seed));

            _testIdPartition = allTestNames
                .Partition(partitionSize)
                .ElementAt(partitionIndex);
        }

        public override TNode AddToXml(TNode parentNode, bool recursive)
        {
            throw new NotImplementedException();
        }

        public override bool Match(ITest test)
        {
            return _testIdPartition.Contains(test.MethodName);
        }
    }
}
