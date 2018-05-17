# Hive-IT
Hive IT is a content management system to function as a back-end for a imaginary mobile device repair company. Hive IT was created in 
asp.net core 2.0 and C# using Visual Studio 2017. It allows for the creation and maintenance of users, customers and work orders on a
basic user level. For admins creation and maintenance of content is extended. The database when created has a default user of "defaultuser"
and password of "password" with the intention that it is just a temporary user to be discarded as soon as another admin is created.

NOTE: If having issues getting the program to run, delete the migration files and type the following in NuGet console:
Add-Migration InitialCreate -context ApplicationDataContext
( Then when that is finished) Add-Migration InitialCreate -context CustomerDataContext
