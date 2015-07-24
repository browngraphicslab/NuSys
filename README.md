# NuSys
Manually using richtextbox
=================================
1. Create Resources.resw file
2. Add .nusys file as resource
3. Initialize the richtextbox like so
```c#
var nodeVm = _factory.CreateNewRichText(readFile);  
this.PositionNode(nodeVm, 100, 100);
NodeViewModelList.Add(nodeVm);
AtomViewList.Add(nodeVm.View);
```

4. Add StreamReader to Factory and pass in the file in our case the file was called 'paragraph'

```c#
using (Stream s =
      typeof(NuSysApp.App).GetTypeInfo()
          .Assembly.GetManifestResourceStream("NuSysApp.Assets.paragraph.nusys"))
 {
    StreamReader reader = new StreamReader(s);
    c = reader.ReadToEnd();
    Debug.WriteLine(c);
 }

