cd "C:\Users\Zack\Documents\Visual Studio 2012\Projects\Eve\EProfit"
robocopy scripts "C:\inetpub\wwwroot\EProfit\scripts"
robocopy bin "C:\inetpub\wwwroot\EProfit\bin"
robocopy images "C:\inetpub\wwwroot\EProfit\images"
robocopy Patches "C:\inetpub\wwwroot\EProfit\patches" /E
robocopy Content "C:\inetpub\wwwroot\EProfit\Content" /E
robocopy Views "C:\inetpub\wwwroot\EProfit\Views" /E
robocopy . "C:\inetpub\wwwroot\EProfit" web.config *.js *.asax *.aspx *.css *.xml