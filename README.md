# NuSys
Manually using richtextbox
=================================
1. Create Resources.resw file
2. Add .nusys file as resource
3. Initialize the richtextbox like so: `var nodeVm = _factory.CreateNewRichText(readFile);
                    this.PositionNode(nodeVm, 100, 100);
                    NodeViewModelList.Add(nodeVm);
                    AtomViewList.Add(nodeVm.View);`
