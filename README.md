# XertExplorer
This is a UI application. The primary objective of the app is to make an improved tool to find appropriate Xert workouts. 

### About Xert
Xert is a tool for cyclists and runners that analyzes performance metrics from data uploaded into their system. With this analysis, Xert gives insight into training and recovery science. One of the tools  available to users is the Xert Training Advisor that gives guidance for workout selection based on the user's current fitness signature.   
[Xert web site](https://www.xertonline.com/)

### Application Technical Details
This is a WPF application written using the MVVM design pattern. It uses a dll I wrote [XertClient](https://github.com/ccuddohy/XertClient) that wraps the public
Xert API. Both the app and the dll were written using .Net core 3.1. 

##### Model / ViewModel
The model in this application is a List of IXertWorkouts which is an object type I created in the XertClient dll. Elements of the List are never modified in the application. The ViewModel modifies the workout list presented to the view by filtering and sorting based on criteria the user enters. 

##### Unit Testing
I broke one of my fundamental rules with this appliation. There is no unit testing at this time. My primary goal for writing this application was to gain a better understanding of WPF and the MVVM design pattern. The only testing I did was exercising the functionality. If there is any reason to extend the use of this application, unit testing should be seriously considered.

##### Comments
It seems that the WPF and MVVM are a giant step forwared over traditional form applications for .NET UI development.

