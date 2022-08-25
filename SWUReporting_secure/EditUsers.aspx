<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true"   AutoEventWireup="true" CodeBehind="EditUsers.aspx.cs" Inherits="EditUsers" %>

<%@ Register Src="~/EditLearnersControl.ascx" TagPrefix="uc1" TagName="ELC" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">    
    <asp:Button ID="btnTesting" runat="server" Text="Button" OnClick="btnTesting_Click" Visible ="false"/>
    <div class="form-inline">
    <br />
    <asp:TextBox ID="tbSearch" runat="server" CssClass="form-control" placeholder="Search"/>
    <asp:Button ID="btnSearch" runat="server" CssClass="btn" OnClick="btnSearch_Click" Text="Search" />
</div>
<div class="form-inline">
    <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy2" runat="server" style="display:none" />
</div>
    <br />
    <uc1:ELC runat="server" ID="elc2" />
</asp:Content>   


