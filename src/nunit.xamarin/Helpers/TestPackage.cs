// ***********************************************************************
// Copyright (c) 2017 NUnit Project
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using nunit.xamarin.Helpers;
using NUnit.Framework;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Runner.Services;

namespace NUnit.Runner.Helpers
{
    /// <summary>
    /// Contains all assemblies for a test run, and controls execution of tests and collection of results
    /// </summary>
    internal class TestPackage
    {
        private readonly List<(Assembly, Dictionary<string, object>)> _testAssemblies = new List<(Assembly, Dictionary<string, object>)>();

        public void AddAssembly(Assembly testAssembly, Dictionary<string, object> options = null)
        {
            _testAssemblies.Add((testAssembly, options));
        }

        public void ClearAssemblies()
        {
            _testAssemblies.Clear();
        }

        public async Task<TestRunResult> ExecuteTests()
        {
            var resultPackage = new TestRunResult();

            foreach (var (assembly, options) in _testAssemblies)
            {
                var runner = await LoadTestAssemblyAsync(assembly, options).ConfigureAwait(false);
                var testCaseCount = runner.CountTestCases(TestFilter.Empty);
                var testNames = new HashSet<string>();

                foreach (var type in assembly.GetTypes())
                {
                    foreach (var methodInfo in type.GetMethods())
                    {
                        var attributes = methodInfo.GetCustomAttributes(true);
                        foreach (var attr in attributes)
                        {
                            if (attr is NUnitAttribute)
                            {
                                var methodName = methodInfo.Name;
                                testNames.Add(methodName);
                            }
                        }
                    }
                }


                var partitionCount = (int)options[nameof(TestOptions.TotalPartitionCount)];
                var partitionIndex = (int)options[nameof(TestOptions.PartitionIndex)];

                Assert.That(testCaseCount, Is.GreaterThanOrEqualTo(testNames.Count));
                Assert.That(partitionIndex, Is.LessThan(partitionCount).And.GreaterThanOrEqualTo(0));

                var filter = partitionCount > 1 ?
                        new PartitionTestFilter(
                            testNames,
                            partitionIndex,
                            partitionCount,
                            (int)options[nameof(TestOptions.PartitionRandomizerSeed)])
                        : TestFilter.Empty;

                var result = await Task.Run(() => runner.Run(TestListener.NULL, filter)).ConfigureAwait(false);
                resultPackage.AddResult(result);
            }

            resultPackage.CompleteTestRun();
            return resultPackage;
        }

        private static async Task<NUnitTestAssemblyRunner> LoadTestAssemblyAsync(Assembly assembly, Dictionary<string, object> options)
        {
            var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
            await Task.Run(() => runner.Load(assembly, options ?? new Dictionary<string, object>()));
            return runner;
        }
    }
}
