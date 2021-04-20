using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EightPuzzle {
	public class Program {
		class Foo : IComparable<Foo> {
			public int Value;
			public Foo(int value = default) => Value = value;
			public int CompareTo(Foo other) {
				Console.WriteLine($"Comparing {Value} and {other.Value}");
				return Value.CompareTo(other.Value);
			}
		}
		static void Main() {
			var heap = new C5.IntervalHeap<Foo>();
			var foos = new Foo[] { new Foo(0), new Foo(1), new Foo(2) };
			foreach (var foo in foos)
				heap.Add(foo);
		}
	}
}
