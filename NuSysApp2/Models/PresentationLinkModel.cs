﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
{
    public class PresentationLinkModel
    {
        // element controller ids / element model id
        public string InElementId { get; set; }
        public string OutElementId { get; set; }

        public ElementViewModel InElementViewModel
        {
            get
            {
                Debug.Assert(InElementId != null);
                var elementViewModels = SessionController.Instance.ActiveFreeFormViewer.AllContent.Where(elementVM => elementVM.Id == InElementId).ToList();
                Debug.Assert(elementViewModels != null);
                Debug.Assert(elementViewModels.Count == 1); // we shouldn't have multiple
                return elementViewModels.First();
            }
        }

        public ElementViewModel OutElementViewModel
        {
            get
            {
                Debug.Assert(OutElementId != null);
                var elementViewModels = SessionController.Instance.ActiveFreeFormViewer.AllContent.Where(elementVM => elementVM.Id == OutElementId).ToList();
                Debug.Assert(elementViewModels != null);
                Debug.Assert(elementViewModels.Count == 1); // we shouldn't have multiple
                return elementViewModels.First();
            }
        }
    }
}