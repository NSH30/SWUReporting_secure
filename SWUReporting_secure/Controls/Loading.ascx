<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Loading.ascx.cs" Inherits="SWUReporting.Loading" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ctk" %>

<asp:Panel ID="pnlLoading" runat="server">
    <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #ffffff; opacity: 0.5;">
        <asp:Image ID="imgUpdateProgress" runat="server" ImageUrl="~/Images/Fadingballs.gif" AlternateText="Loading ..." ToolTip="Loading ..." style="padding: 10px;position:fixed;top:45%;left:50%;" />
    </div>
</asp:Panel>