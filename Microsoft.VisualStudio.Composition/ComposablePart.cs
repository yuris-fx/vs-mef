﻿namespace Microsoft.VisualStudio.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Validation;

    [DebuggerDisplay("{Definition.Type.Name}")]
    public class ComposablePart
    {
        public ComposablePart(ComposablePartDefinition definition, IReadOnlyDictionary<Import, IReadOnlyList<Export>> satisfyingExports)
        {
            Requires.NotNull(definition, "definition");
            Requires.NotNull(satisfyingExports, "satisfyingExports");

            // Make sure we have entries for every import.
            Requires.Argument(satisfyingExports.Count == definition.ImportDefinitions.Count() && definition.ImportDefinitions.All(d => satisfyingExports.Keys.Any(e => e.ImportDefinition.Equals(d))), "satisfyingExports", "There should be exactly one entry for every import.");
            Requires.Argument(satisfyingExports.All(kv => kv.Value != null), "satisfyingExports", "All values must be non-null.");

            this.Definition = definition;
            this.SatisfyingExports = satisfyingExports;
        }

        public ComposablePartDefinition Definition { get; private set; }

        public IReadOnlyDictionary<Import, IReadOnlyList<Export>> SatisfyingExports { get; private set; }

        public IEnumerable<KeyValuePair<Import, IReadOnlyList<Export>>> GetImportingConstructorImports()
        {
            foreach (var importDefinition in this.Definition.ImportingConstructor)
            {
                var key = this.SatisfyingExports.Keys.Single(k => k.ImportDefinition == importDefinition);
                yield return new KeyValuePair<Import, IReadOnlyList<Export>>(key, this.SatisfyingExports[key]);
            }
        }

        public void Validate()
        {
            foreach (var pair in this.SatisfyingExports)
            {
                var importDefinition = pair.Key.ImportDefinition;
                switch (importDefinition.Cardinality)
                {
                    case ImportCardinality.ExactlyOne:
                        Verify.Operation(pair.Value.Count == 1, "Import of {0} expected 1 export but found {1}.", importDefinition.Contract, pair.Value.Count);
                        break;
                    case ImportCardinality.OneOrZero:
                        Verify.Operation(pair.Value.Count < 2, "Import of {0} expected 1 or 0 exports but found {1}.", importDefinition.Contract, pair.Value.Count);
                        break;
                }

                foreach (var export in pair.Value)
                {
                    var receivingType = pair.Key.ImportDefinition.ElementType;
                    if (export.ExportedValueType.IsGenericTypeDefinition && receivingType.IsGenericType)
                    {
                        receivingType = receivingType.GetGenericTypeDefinition();
                    }

                    Verify.Operation(
                        receivingType.IsAssignableFrom(export.ExportedValueType),
                        "Exporting MEF part {0} is not assignable to {1}, as required by import found on {2}.{3}",
                        export.PartDefinition.Type.Name,
                        importDefinition.MemberType.Name,
                        this.Definition.Type.Name,
                        pair.Key.ImportingMember != null ? pair.Key.ImportingMember.Name : "ctor");
                }
            }
        }
    }
}
