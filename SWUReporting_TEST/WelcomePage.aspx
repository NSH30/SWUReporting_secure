<%@ Page Title="SWU Reporting - Welcome" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="WelcomePage.aspx.cs" Inherits="SWUReporting.WelcomePage" %>
<asp:Content ID="BodyCOntent" ContentPlaceHolderID="MainContent" runat="server">   

<script src="/SWUReporting/bundles/MsAjaxJs?v=c42ygB2U07n37m_Sfa8ZbLGVu4Rr2gsBo7MvUEnJeZ81" type="text/javascript"></script>
<script src="Scripts/jquery-1.10.2.min.js" type="text/javascript"></script>
<script src="Scripts/bootstrap.min.js" type="text/javascript"></script>
<script src="Scripts/respond.min.js" type="text/javascript"></script>
<script src="/SWUReporting/bundles/WebFormsJs?v=AAyiAYwMfvmwjNSBfIMrBAqfU5exDukMVhrRuZ-PDU01" type="text/javascript"></script>

        <div class="navbar navbar-inverse navbar-fixed-top">
            <div class="container">
                <div class="navbar-header">
                    <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-collapse">
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>
                    <a class="navbar-left"><img src="Images/SwuLogo.png"  style="height: 44px; width: 150px; margin-top: 10px; margin-right: 10px" /> </a>
                </div>                                       
            </div>
        </div>    
        <div class="container body-content">

            <div class="container p-5 shadow" style="background-color:whitesmoke; margin:80px 80px 200px 80px; padding-bottom:30px ">
        <div>
            <h3 style="margin:40px 50px 40px 50px">Welcome to  SOLIDWORKS Reporting Tool</h3>            
        </div>
        <div >
            <br />
            <asp:Button  ID="Enter" Text="Enter" CssClass="btn btn-primary" runat="server" Style="margin:10px 0px 0px 400px" OnClick="Enter_click" />
        </div>  
        <br />              
    </div>
          
        </div>

</asp:Content>
