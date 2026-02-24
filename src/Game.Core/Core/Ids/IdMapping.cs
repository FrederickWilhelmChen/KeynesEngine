using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Core.Ids
{
    public sealed class IdMapping
    {
        public IReadOnlyList<string> Codes { get; }
        public IReadOnlyDictionary<string, int> CodeToId { get; }

        private IdMapping(List<string> codes, Dictionary<string, int> codeToId)
        {
            Codes = codes;
            CodeToId = codeToId;
        }

        public static IdMapping Create(IEnumerable<string> codes)
        {
            var codeList = codes
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim())
                .OrderBy(code => code, StringComparer.Ordinal)
                .ToList();

            if (codeList.Count == 0)
            {
                throw new InvalidOperationException("No codes provided.");
            }

            var codeToId = new Dictionary<string, int>(codeList.Count, StringComparer.Ordinal);
            for (var i = 0; i < codeList.Count; i++)
            {
                var code = codeList[i];
                if (codeToId.ContainsKey(code))
                {
                    throw new InvalidOperationException($"Duplicate code detected: {code}");
                }

                codeToId[code] = i;
            }

            return new IdMapping(codeList, codeToId);
        }
    }
}
