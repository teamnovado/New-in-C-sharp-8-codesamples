using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NewInCSharp8
{
    class Program
    {
        static async Task Main(string[] args)
        {

            DefaultInterfaceMethodsDemo();
            RangesAndIndicesDemo();
            await AsyncEnumerablesDemo();
            RecursivePatternsDemo();

        }

    #region DefaultInterfaceMethods
        private static void DefaultInterfaceMethodsDemo()
        {
            //Statics and default implementations on interfaces are only accessible through the interface type itself
            //This behavior is similar to explicitly implemented methods

            //Concretely, the following will not compile
            //var f = new Foo ();
            //f.Bar();
            //f.SetRepeatCount (1);


            IFoo foo = new Foo();
            Console.WriteLine($"Bar: {foo.Bar()}");
            Console.WriteLine($"RepeatedBar: {foo.RepeatedBar()}");

            Console.WriteLine(IFoo.RepeatCount);
            IFoo.SetRepeatCount(4);
            
            Console.WriteLine($"RepeatedBar: {foo.RepeatedBar()}");
        }

        interface IFoo
        {
            //Interfaces methods can have bodies
            string Bar()
            {
                return "Bar";
            }

            //Interfaces can reference only other interface methods & statics (fields, properties, methods) defined on the interface
            string RepeatedBar()
            {
                return Enumerable.Repeat(Bar(), RepeatCount).Aggregate((a, v) => a + v);
            }

            //Interfaces can have static fields/properties
            public static int RepeatCount { get; private set; } = 2;

            //Interfaces can have static methods
            public static void SetRepeatCount(int value)
            {
                RepeatCount = value;
            }
        }

        class Foo : IFoo { }
    #endregion

    #region AsyncEnumerable 
    static async Task AsyncEnumerablesDemo ()
        {
            //Note the await before the foreach.
            //This will cause the compiler to await the MoveNextAsync method on the AsyncEnumerator
            await foreach (var x in new AsyncEnum ())
            {
                Console.WriteLine(x);
            }

        }

        //Note the new interface 'IAsyncEnumerable' vs 'IEnumerable'
        //Also note there's no non-generic GetAsyncEnumerator
        class AsyncEnum : IAsyncEnumerable<int>
        {
            public async IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                for (int x = 0; x< 10; x++)
                {
                    await Task.Delay(500);
                    yield return x;
                }
                
            }

        }
        #endregion

    #region RangesAndIndices + static local functions
        static void RangesAndIndicesDemo ()
        {
            static void DumpArray (string caption, int[] data)
            {
                DumpValue (caption, data.Select (v => v.ToString()).Aggregate((a, v) => $"{a}, {v}"));
            }

            static void DumpValue(string caption, object value)
            {
                Console.WriteLine($"{caption}: {value}");
            }


            var numbers = Enumerable.Range(0, 10).ToArray();

            var i = ^4; //is sugar for new Index (4, true)
            var range = 2..4; // is sugar for new Range (new Index (2, false), new Index (4, false))
            

            DumpValue ("^4", numbers[i]);

            //Note that ranges are inclusive the lower range but exclusive the upper range
            DumpArray ("[0..3]", numbers[0..3]);
            DumpArray("[2..4]", numbers[range]);
            DumpArray("6..^0]", numbers[6..^0]);
        }
        #endregion


    #region RecursivePatternsDemo
        public static void RecursivePatternsDemo()
        {
            var objs = new object[] { new PointOfInterest { Name = "test", Location = new Point { X = 2, Y = 3 } },
                new Point { X =2, Y = 2 },
                new Point {X = 42, Y = 1},
                new PointOfInterest(),
                new PointOfInterest {Location = new Point {X=0,Y=2}},
                "test",
                null
            };

            var results = objs.Select (o => o switch
            {
                //Recursive pattern: Type pattern + positional pattern + post patternmatch conditional
                Point(var x, var y) when x == 2 => "point with X coordinate 2",
                
                //Recursive pattern: Type pattern + positional pattern
                Point(var x, var y) => $"Generic Point with coords ({x},{y})",

                //Recursive pattern: Type pattern + property pattern
                PointOfInterest { Name: "test"} => "specific poi named test",

                //Recursive pattern: Type pattern + property pattern + positional pattern + post patternmatch conditional
                PointOfInterest { Location: (var x, var y)} when y == 2 => "specific point of interest with Y coordinate 2",
                
                //Non-recursive pattern: Type pattern
                PointOfInterest p => "generic point of interest",

                //Non-recursive pattern: empty property pattern
                { } => "an object",
                
                //Not a pattern, discard feature from C# 7.3
                _ => "nothing!"
            });

            foreach (var result in results)
                Console.WriteLine(result);
            
        }

        class PointOfInterest
        {
            public string Name;
            public Point Location; 
        }

        class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
            public void Deconstruct (out int x, out int y)
            {
                x = X;
                y = Y;
            }
        }
        #endregion

#nullable enable

        static void NullableReferencesDemo ()
        {

            string test = null; // Note the warning due to assigning null to a non-nullable reference type
            string? test2 = null;
            string result; 

            result = Foo(test); //Note no warning due to flow analysis confirming that return value is guaranteed non-null
            result = Bar(test);
            result = Baz(test);

            string Foo (string? input)
            {
                return input ?? string.Empty;
            }

            string Bar (string? input)
            {
                var isnull = input == null;

                //Note that not every case where non-nullness is guaranteed is found by flow analysis
                return isnull ? string.Empty : input; 
            }

            string Baz(string? input)
            {
                var isnull = input == null;

                //Note the ! operator that can be used to explicitly indicate the value is guaranteed non-null
                return isnull ? string.Empty : input!;
            }

            var fb = new FooBar<string> ();
            var fb2 = new FooBar<string?>(); //Note the warning on the parameter type due to the notnull constraint

            
        }

        //Note the use of attributes to give specific information about nullness of values 
        [return: MaybeNull]
        static string DoNothing([AllowNull]string value)
        {
            return value; 
        }


        class FooBar<T> where T:notnull
        {
        }
#nullable disable

    }
}
