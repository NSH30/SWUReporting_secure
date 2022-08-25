<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TransposedReport.ascx.cs" Inherits="SWUReporting.TransposedReport" %>
<p>
    <asp:Label ID="Label1" runat="server" Text="Company Filter: "></asp:Label>
    <asp:TextBox ID="txtVARFilter" runat="server" ToolTip="Use % as a wildcard. ie: hawk% for Hawk Ridge or % for all VARs" Width="214px"></asp:TextBox>
</p>
<p>
    <asp:CheckBox ID="chkReseller" runat="server" Checked="True" Text="Reseller" />
    <asp:CheckBox ID="chkEmployee" runat="server" Text="Employee" />
</p>
<p>
    <asp:Button ID="btnDownload" runat="server" OnClick="btnDownload_Click" Text="Download"/>
<asp:SqlDataSource ID="SWUTable" runat="server" ConnectionString="<%$ ConnectionStrings:SWU %>" SelectCommand="uspTransposedReport" SelectCommandType="StoredProcedure">
    <SelectParameters>
        <asp:Parameter DefaultValue="goeng%" Name="VARFilter" Type="String" />
        <asp:Parameter DefaultValue="resell%" Name="ProfileFilter" Type="String" />
    </SelectParameters>
</asp:SqlDataSource>
</p>
<p>
    <%--    &nbsp;</p>
<asp:GridView ID="gvData" runat="server" DataSourceID="SWUTable">
</asp:GridView>--%>