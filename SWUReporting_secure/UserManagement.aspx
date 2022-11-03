<%@ Page Title="SWU Reporting - User Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="SWUReporting.UserManagement" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h4>User/Admin Management</h4>
      
        <div>  
            <table class="auto-style1">  
                <tr>  
                    <td>First Name:</td>  
                    <td>  
                        <asp:TextBox ID="fname_text" runat="server"></asp:TextBox>  
                    </td>    
               </tr>
                <tr>  
                    <td>Last Name:</td>  
                    <td>  
                        <asp:TextBox ID="lname_text" runat="server"></asp:TextBox>  
                    </td>  
  
               </tr>     
                <tr>  
                    <td>Trigram</td>  
                    <td>  
                        <asp:TextBox ID="Trig_text" runat="server"></asp:TextBox>  
                    </td>  
                </tr>
                <tr>  
                    <td>Admin</td>  
                    <td>  
                        <asp:RadioButtonList ID="Admin_radio" runat="server">  
                            <asp:ListItem>Yes</asp:ListItem>  
                            <asp:ListItem>No</asp:ListItem>  
                        </asp:RadioButtonList>  
                    </td>  
               </tr>  
                <tr>                    
                    <td>  
                        <asp:Button ID="Submit" runat="server" Text="Submit" />  
                    </td>  
                </tr>  
            </table>  
        </div>  
    
</asp:Content>


