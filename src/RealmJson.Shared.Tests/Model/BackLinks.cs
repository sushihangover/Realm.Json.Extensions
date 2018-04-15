using System;
using System.Collections.Generic;
using System.Linq;
using Realms;

namespace RealmJson.Test
{
	[Preserve(AllMembers = true)]
	public class A : RealmObject
	{
		[PrimaryKey]
		public string Id { get; set; } = Guid.NewGuid().ToString();
		[Backlink(nameof(B.A))]
		public IQueryable<B> BList { get; }
	}

	[Preserve(AllMembers = true)]
	public class B : RealmObject
	{
		[PrimaryKey]
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public A A { get; set; }
		[Backlink(nameof(C.B))]
		public IQueryable<C> CList { get; }
	}

	[Preserve(AllMembers = true)]
	public class C : RealmObject
	{
		[PrimaryKey]
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public B B { get; set; }
	}
}
