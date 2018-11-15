<!-- default file list -->
*Files to look at*:

* [GridSelectingBehavior.cs](./CS/DXGridSample/GridSelectingBehavior.cs) (VB: [GridSelectingBehavior.vb](./VB/DXGridSample/GridSelectingBehavior.vb))
* [SampleSource1.cs](./CS/DXGridSample/SampleSource1.cs) (VB: [SampleSource1.vb](./VB/DXGridSample/SampleSource1.vb))
* [ScrollController.cs](./CS/DXGridSample/ScrollController.cs) (VB: [ScrollController.vb](./VB/DXGridSample/ScrollController.vb))
* [Window1.xaml](./CS/DXGridSample/Window1.xaml) (VB: [Window1.xaml](./VB/DXGridSample/Window1.xaml))
* [Window1.xaml.cs](./CS/DXGridSample/Window1.xaml.cs) (VB: [Window1.xaml](./VB/DXGridSample/Window1.xaml))
<!-- default file list end -->
# How to scroll TableView on selecting row with moving the mouse outside view bounds


<p><strong>Obsolete.</strong> Starting with 12.1, to achieve this functionality, set the GridControl.SelectionMode property to Cell. </p>
<p><br><br><br>This example illustrates how to add the DXGrid behavior which provides the capability to select rows and cells by simply moving the mouse over them with the mouse button pressed and auto-scrolling the view in a necessary direction.</p>
<p>This functionality was implemented via attached behavior for DXGrid which encapsulates all the selection functionality.</p>
<p>The scrolling functionality was implemented in a separate class named ScrollController.</p>

<br/>


