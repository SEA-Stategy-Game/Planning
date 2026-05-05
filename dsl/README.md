## To run the project
Navigate to the folder with the dsl project file.
If dotnet 10 is installed, the following can be used to run the project:
    dotnet run test.txt
where test.txt is the plan supplied to the dsl validator

## To compile the project
Again navigate to the folder with the project file.
Running the following creates a binary file that can be used to validate plans:
    dotnet publish -o binary
which create a directory called binary that has an executable file called dsl.

This can be run using:
    ./dsl test.txt
The validated plan gets saved as a plan.json file, aswell as attempt to send
the file to the planning backend.
