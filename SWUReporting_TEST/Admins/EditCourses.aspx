<%@ Page Language="C#" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" CodeBehind="EditCourses.aspx.cs" Inherits="SWUReporting_TEST.EditCourses"  MasterPageFile="~/Site.Master" %>
<%@ Register Src="~/Controls/EditCourseControl.ascx" TagPrefix="ecc9" TagName="EditCourseControl" %>
<%@ Register Src="~/Controls/EditCourseAliasControl.ascx" TagPrefix="ecac1" TagName="EditCourseAliasControl" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">        
    <br />
        <table style="width:100%">
            <tr>
                <td  style="width:50%">
                    <asp:Panel CssClass="form-inline" ID="CourseSearch" runat="server" DefaultButton="lnkSearch">
                        <asp:TextBox ID="tbSearch" runat="server" CssClass="form-control" Width="50%" Placeholder="Search"/>
                        <%--<asp:Button ID="btnSearch" runat="server" CssClass="btn" OnClick="btnSearch_Click" Text="Search" />--%>
                        <asp:LinkButton runat="server" ID="lnkSearch" CssClass="btn form-inline" placeholder="Search" OnClick="btnSearch_Click">
                            <span aria-hidden="true" class="btn glyphicon glyphicon-search" />
                        </asp:LinkButton>
                    </asp:Panel>
                </td>
                <td>
                    <div class="form-inline">
                    <asp:Button ID="btnAlias" runat="server" CssClass="btn" Text="Course Alias List" OnClick="btnAlias_Click" />                        
                    </div>
                </td>
            </tr>
        </table>
    <br />
<div class="form-inline">
    <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy2" runat="server" style="display:none" />
</div>
    <br />
    <ecc9:EditCourseControl runat="server" ID="editCourseControl" Visible="false" />
    <ecac1:EditCourseAliasControl runat="server" ID="editCourseAliasControl" Visible="false" />
</asp:Content> 
