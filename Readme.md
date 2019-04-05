<!-- default file list -->
*Files to look at*:

* [Window1.xaml](./CS/DXGridSample/Window1.xaml) (VB: [Window1.xaml](./VB/DXGridSample/Window1.xaml))
* [Window1.xaml.cs](./CS/DXGridSample/Window1.xaml.cs) (VB: [Window1.xaml.vb](./VB/DXGridSample/Window1.xaml.vb))
<!-- default file list end -->
# How to scroll TableView on selecting row with moving the mouse outside view bounds


<p><strong>Obsolete.</strong> Starting with 12.1, to achieve this functionality, set the GridControl.SelectionMode property to Cell. </p>
<p><br><br><br>This example illustrates how to add the DXGrid behavior which provides the capability to select rows and cells by simply moving the mouse over them with the mouse button pressed and auto-scrolling the view in a necessary direction.</p>
<p>This functionality was implemented via attached behavior for DXGrid which encapsulates all the selection functionality.</p>
<p>The scrolling functionality was implemented in a separate class named ScrollController.</p>


<h3>Description</h3>

<p>This example demonstrates how to select rows by simply moving the mouse over them with the mouse button pressed</p>

<br/>


