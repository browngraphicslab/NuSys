using System.Collections.Generic;

namespace NuSysApp.Misc.SpeechToTextUI
{
    class MyLcsAlgo
    {

        public static LongestCommonSubstringResult FindLongestCommonSubstring(
            string firstCollection, int firstStart, int firstEnd,
            string secondCollection, int secondStart, int secondEnd)
        {
            // check for empty 
            if (string.IsNullOrEmpty(firstCollection) || string.IsNullOrEmpty(secondCollection)
                || firstEnd - firstStart < 1 || secondEnd - secondStart < 1)
            {
                return new LongestCommonSubstringResult();
            }

            // create a table to store lengths of longest common suffixes of
            // substrings. note that lcsuff[i][j] contains the length of
            // the longest common suffix of X[0...i-1] Y[0...j-1]. The first
            // row and first column entries have no logical meaning
            int[,] LCSuff = new int[firstEnd - firstStart + 1,secondEnd - secondStart + 1];
            // store the starting position of each suffix
            int[,,] firstPos_secondPos = new int[firstEnd - firstStart + 1, secondEnd - secondStart + 1, 2];
            int length = 0; // store length of longest common substring
            int firstPos = firstStart;
            int secondPos = secondStart;

            /* build LCSuff[firstEnd][secondEnd] in bottom up way */
            for (int i = 0; i <= firstEnd - firstStart; i++)
            {
                for (int j = 0; j <= secondEnd - secondStart; j++)
                {
                    if (i == 0 || j == 0)
                    {
                        LCSuff[i, j] = 0;
                        firstPos_secondPos[i, j , 0] = firstStart + i;
                        firstPos_secondPos[i, j, 1] = secondStart + j;
                    }
                    else if (firstCollection[firstStart + i - 1] == secondCollection[secondStart + j - 1])
                    {
                        LCSuff[i, j] = LCSuff[i - 1, j - 1] + 1;
                        firstPos_secondPos[i, j,0] = firstPos_secondPos[i - 1, j - 1,0];
                        firstPos_secondPos[i, j, 1] = firstPos_secondPos[i - 1, j - 1, 1];
                        if (LCSuff[i, j] > length)
                        {
                            firstPos = firstPos_secondPos[i, j,0];
                            secondPos = firstPos_secondPos[i, j, 1];
                            length = LCSuff[i, j];
                        }
                    }
                    else
                    {
                        LCSuff[i, j] = 0;
                        firstPos_secondPos[i, j, 0] = firstStart + i;
                        firstPos_secondPos[i, j, 1] = secondStart + j;
                    }
                }
            }
            // only return a LongestcommonSubstringResult if length is > 0
            // default struct produces Success=false
            if (length > 0)
            {
                // create the LongestCommonSubstringResult
                return new LongestCommonSubstringResult(
                    firstPos,
                    secondPos,
                    length);
            }
            else
            {
                return new LongestCommonSubstringResult();
            }
            
        }

        //outputs diffsections of two strings in delete, insert, copy order
        public static IEnumerable<DiffSection> Diff(
    string firstCollection, int firstStart, int firstEnd,
    string secondCollection, int secondStart, int secondEnd)
        {
            var lcs = FindLongestCommonSubstring(
                firstCollection, firstStart, firstEnd,
                secondCollection, secondStart, secondEnd);

            if (lcs.Success)
            {
                // deal with the section before
                var sectionsBefore = Diff(
                    firstCollection, firstStart, lcs.PositionInFirstCollection,
                    secondCollection, secondStart, lcs.PositionInSecondCollection);
                foreach (var section in sectionsBefore)
                    yield return section;

                // output the copy operation
                yield return new DiffSection(
                    DiffSectionType.Copy,
                    lcs.Length, 
                    firstCollection.Substring(lcs.PositionInFirstCollection, lcs.Length));

                // deal with the section after
                var sectionsAfter = Diff(
                    firstCollection, lcs.PositionInFirstCollection + lcs.Length, firstEnd,
                    secondCollection, lcs.PositionInSecondCollection + lcs.Length, secondEnd);
                foreach (var section in sectionsAfter)
                    yield return section;

                yield break;
            }

            if (firstStart < firstEnd)
            {
                // we got content from first collection --> deleted
                yield return new DiffSection(
                    DiffSectionType.Delete,
                    firstEnd - firstStart,
                    firstCollection.Substring(firstStart, firstEnd - firstStart));
            }
            if (secondStart < secondEnd)
            {
                // we got content from second collection --> inserted
                yield return new DiffSection(
                    DiffSectionType.Insert,
                    secondEnd - secondStart,
                    secondCollection.Substring(secondStart, secondEnd - secondStart));
            }

        }

        public struct LongestCommonSubstringResult
        {
            private readonly bool _Success;
            private readonly int _PositionInFirstCollection;
            private readonly int _PositionInSecondCollection;
            private readonly int _Length;

            public LongestCommonSubstringResult(
                int positionInFirstCollection,
                int positionInSecondCollection,
                int length)
            {
                _Success = true;
                _PositionInFirstCollection = positionInFirstCollection;
                _PositionInSecondCollection = positionInSecondCollection;
                _Length = length;
            }

            public bool Success
            {
                get
                {
                    return _Success;
                }
            }

            public int PositionInFirstCollection
            {
                get
                {
                    return _PositionInFirstCollection;
                }
            }

            public int PositionInSecondCollection
            {
                get
                {
                    return _PositionInSecondCollection;
                }
            }

            public int Length
            {
                get
                {
                    return _Length;
                }
            }

            public override string ToString()
            {
                if (Success)
                    return string.Format(
                        "LCS ({0}, {1} x{2})",
                        PositionInFirstCollection,
                        PositionInSecondCollection,
                        Length);
                else
                    return "LCS (-)";
            }
        }

        public enum DiffSectionType
        {
            Copy,
            Insert,
            Delete
        }

        public struct DiffSection
        {
            private readonly DiffSectionType _Type;
            private readonly int _Length;
            private readonly string _Content;

            public DiffSection(DiffSectionType type, int length)
            {
                _Type = type;
                _Length = length;
                _Content = null;
            }

            public DiffSection(DiffSectionType type, int length, string content)
            {
                _Type = type;
                _Length = length;
                _Content = content;
            }

            public DiffSectionType Type
            {
                get
                {
                    return _Type;
                }
            }

            public int Length
            {
                get
                {
                    return _Length;
                }
            }

            public string Content
            {
                get
                {
                    return _Content;
                }
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2}", Type, Length, Content);
            }
        }
    }
}

