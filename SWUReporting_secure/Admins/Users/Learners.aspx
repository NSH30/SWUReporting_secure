<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true"  AutoEventWireup="true" CodeBehind="Learners.aspx.cs" Inherits="SWUReporting.Learners" %>

<%@ Register Src="/Controls/EditLearnersControl.ascx" TagPrefix="uc1" TagName="EditLearnersControl" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <br />
    <div class="form-inline">
        
    </div>   
    <table style="width:100%">
        <tr>
            <td  style="width:70%">
                <asp:Panel ID="SearchLearners" runat="server" CssClass="form-inline" DefaultButton="lnkSearch">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="form-control" placeholder="Search"></asp:TextBox>
                    <%--<asp:Button ID="btnSearch" runat="server" CssClass="btn" OnClick="btnSearch_Click" Text="Search" />--%>
                    <asp:LinkButton runat="server" ID="lnkSearch" CssClass="btn form-inline" placeholder="Search" OnClick="btnSearch_Click">
                        <span aria-hidden="true" class="btn glyphicon glyphicon-search" />
                    </asp:LinkButton>
                    <asp:CheckBox ID="chkShowDeleted" CssClass="checkbox form-check-label" runat="server" Text="Show DELETED learners"/>
                </asp:Panel>
            </td>
            <td>
                <div class="form-inline">
                    <asp:Button ID="btnShowDuplicates" runat="server" CssClass="btn" style="margin-right:5px" Text="Duplicate Learners" OnClick="btnShowDuplicates_Click" />
                    <asp:Button ID="btnShowUsersMissingData" runat="server" CssClass="btn" Text="Learners Missing Data" OnClick="btnShowUsersMissingData_Click" />
                </div>
            </td>
            <td>
                <div class="form-inline">
                
                </div>
            </td>
        </tr>
    </table> 
    <br />
    <uc1:EditLearnersControl runat="server" ID="elc2" Visible="true"/>
</asp:Content>

