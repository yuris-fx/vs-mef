﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.VisualStudio.Composition.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Composition.Hosting;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    [Trait("SharingBoundary", "")]
    public class SharingBoundaryWithNestedBoundaryFactoryTests
    {
        [MefFact(CompositionEngines.V2Compat)]
        public void SharingBoundaryWithSameNestedBoundary(IContainer container)
        {
            var root = container.GetExportedValue<Root>();
            var boundaryPart = root.SelfFactory.CreateExport().Value;
            var boundaryPartNested = boundaryPart.SelfFactoryWithSharingBoundary.CreateExport().Value;

            Assert.NotSame(boundaryPart, boundaryPartNested);
            Assert.NotSame(boundaryPart.AnotherSharedValue, boundaryPartNested.AnotherSharedValue);
            Assert.Same(boundaryPart, boundaryPart.AnotherSharedValue.FirstSharedPart);
            Assert.Same(boundaryPartNested, boundaryPartNested.AnotherSharedValue.FirstSharedPart);
        }

        [Export]
        public class Root
        {
            [Import, SharingBoundary("A")]
            public ExportFactory<SharingBoundaryPart> SelfFactory { get; set; }
        }

        [Export, Shared("A")]
        public class SharingBoundaryPart
        {
            [Import, SharingBoundary("A")]
            public ExportFactory<SharingBoundaryPart> SelfFactoryWithSharingBoundary { get; set; }

            [Import]
            public AnotherSharedPartInBoundaryA AnotherSharedValue { get; set; }
        }

        [Export, Shared("A")]
        public class AnotherSharedPartInBoundaryA
        {
            [Import]
            public SharingBoundaryPart FirstSharedPart { get; set; }
        }
    }
}
