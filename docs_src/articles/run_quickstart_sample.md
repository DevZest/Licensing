---
uid: run_quickstart_sample
---

# Run QuickStart Sample

This walk-through runs the QuickStart sample previously created.

1. Enable debugging in the web server, by changing `Web.config` of `SamplePublisher` project:
    ```xml
    <configuration>
        ...
        <system.web>
            ...
            <compilation debug="true" />
            ...
        <system.web>
        ...
    </configuration>
    ```
2. Set `SampleApp` as startup project by right clicking `SampleApp` project, then select `Set as StartUp Project`.
3. Press F5 to run the application.