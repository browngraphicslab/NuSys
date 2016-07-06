using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkId
    {
        #region Private Variables
        private string _libraryElementId, _regionId;
        private bool _isRegion;
        #endregion Private Variables
        public LinkId(string libraryElementId, string regionId = null)
        {
            _libraryElementId = libraryElementId;
            _isRegion = false;
            if (regionId != null)
            {
                _isRegion = true;
                _regionId = regionId;
            }
        }
        #region Public Members
        public string LibraryElementId => _libraryElementId;
        public string RegionId => _regionId;
        public bool IsRegion => _isRegion;
        #endregion Public Members
        public static bool operator ==(LinkId x, LinkId y)
        {
            return x.LibraryElementId == y.LibraryElementId && x.RegionId == y.RegionId;
        }

        public static bool operator !=(LinkId x, LinkId y)
        {
            return !(x == y);
        }
        public override int GetHashCode()
        {
            if (RegionId == null)
            {
                return LibraryElementId.GetHashCode();
            }
            return RegionId.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            LinkId other = obj as LinkId;
            if (IsRegion ^ other.IsRegion) //XOR
            {
                return false;
            }
            if (IsRegion)
            {
                return other.LibraryElementId == this.LibraryElementId && other.RegionId == this.RegionId;
            }
            return other.LibraryElementId == this.LibraryElementId;
            
        }
    }
}
