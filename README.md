### Installation Steps:

1. Download the Microsoft package repository configuration file:

    ```bash
    wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    ```

2. Install the Microsoft package repository configuration:

    ```bash
    sudo dpkg -i packages-microsoft-prod.deb
    ```

3. Update the package list:

    ```bash
    sudo apt update
    ```

4. Install the `apt-transport-https` package:

    ```bash
    sudo apt install -y apt-transport-https
    ```

5. Update the package list again:

    ```bash
    sudo apt update
    ```

6. Install the .NET Core SDK version 6.0:

    ```bash
    sudo apt install -y dotnet-sdk-6.0
    ```

7. Verify the installation by checking the .NET Core SDK version:

    ```bash
    dotnet --version
    ```

### Running the ModbusTCPClient Application:

1. Change directory to ModbusTCPClient:

    ```bash
    cd ModbusTCPClient
    ```

2. Build and run the application:

    ```bash
    dotnet run
    ```
