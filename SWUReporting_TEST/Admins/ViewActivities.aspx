<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="ViewActivities.aspx.cs" Inherits="SWUReporting_TEST.ViewActivities" %>

<%@ Register Src="~/Controls/ViewActivitiesControl.ascx" TagPrefix="uc9" TagName="ViewActivitiesControl" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
        <br />
    <div class="form-inline">        
        Starting date: <asp:TextBox ID="tbSearchDate" TextMode="Date" runat="server" CssClass="form-control"></asp:TextBox>
        Ending date: <asp:TextBox ID="tbEndSearchDate" TextMode="Date" runat="server" CssClass="form-control"></asp:TextBox>
        <asp:TextBox ID="tbSearchString" runat="server" CssClass="form-control" placeholder="Search"></asp:TextBox>
        <asp:Button ID="btnSearch" runat="server" CssClass="btn" OnClick="btnSearch_Click" Text="Search" />
        <asp:CheckBox ID="cbCompleted" runat="server" Text="Completed Only"/>
    </div>    
    <br />
    <uc9:ViewActivitiesControl runat="server" id="vac" Visible="false"/>
</asp:Content>