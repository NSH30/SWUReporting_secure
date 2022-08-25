<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewActivitiesControl.ascx.cs" Inherits="SWUReporting.ViewActivitiesControl" %>

<asp:LinkButton ID="lnkSaveExcel" CssClass="btn btn-primary" runat="server" OnClick="btnSaveExcel_Click" style="margin-bottom:10px" ToolTip="Download">
    <span aria-hidden="true" class="glyphicon glyphicon-download-alt"></span>
</asp:LinkButton>
<asp:GridView ID="gvActivities" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="false" AllowSorting="true" AllowPaging="False">
    <Columns>
        
        <asp:BoundField DataField="Name" HeaderText="Name" />
        <asp:BoundField DataField="email" HeaderText="email" />
        <asp:BoundField DataField="VAR_Alias" HeaderText="VAR Name" />
        <asp:BoundField DataField="GEO" HeaderText="GEO" />
        <asp:BoundField DataField="CourseName" HeaderText="Course Name" />
        <asp:BoundField DataField="CourseAlias" HeaderText="Course Alias" />
        <asp:BoundField DataField="Type" HeaderText="Course Type" />
        <asp:BoundField DataField="DomainName" HeaderText="Domain" />
        <asp:BoundField DataField="status" HeaderText="Status" />
        <asp:BoundField DataField="startDate" HeaderText="Start Date" />
        <asp:BoundField DataField="completionDate" HeaderText="Completion Date" />                    
    </Columns>
    
</asp:GridView>
</br>
<asp:Label id="lblMessage" runat="server" Text="" visible ="false"></asp:Label>
