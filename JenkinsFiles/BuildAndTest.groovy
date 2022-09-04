@Library( "X13JenkinsLib" )_

def CallCake( String arguments )
{
    X13Cmd( "./Cake/dotnet-cake ./checkout/build.cake ${arguments}" );
}

def CallDevops( String arguments )
{
    X13Cmd( "dotnet ./checkout/src/DevOps/bin/Debug/net6.0/DevOps.dll ${arguments}" );
}

def Prepare()
{
    X13Cmd( 'dotnet tool update Cake.Tool --version=2.0.0 --tool-path ./Cake' )
    CallCake( "--showdescription" )
}

def Build()
{
    CallCake( "--target=build" );
}

def RunTests()
{
    CallDevops( "--target=run_tests" );
}

def Publish( String target )
{
    CallDevops( "--target=${target}" );
}

pipeline
{
    agent
    {
        label "ubuntu && docker && x64";
    }
    environment
    {
        DOTNET_CLI_TELEMETRY_OPTOUT = 'true'
        DOTNET_NOLOGO = 'true'
    }
    options
    {
        skipDefaultCheckout( true );
    }
    stages
    {
        stage( "Clean" )
        {
            steps
            {
                cleanWs();
            }
        }
        stage( "checkout " )
        {
            steps
            {
                checkout scm;
            }
        }
        stage( "In Dotnet Docker" )
        {
            agent
            {
                docker
                {
                    image 'mcr.microsoft.com/dotnet/sdk:6.0'
                    args "-e HOME='${env.WORKSPACE}'"
                    reuseNode true
                }
            }
            stages
            {
                stage( "Prepare" )
                {
                    steps
                    {
                        Prepare();
                    }
                }
                stage( "Build" )
                {
                    steps
                    {
                        Build();
                    }
                }
                stage( "Run Tests" )
                {
                    steps
                    {
                        RunTests();
                    }
                    post
                    {
                        always
                        {
                            X13ParseMsTestResults(
                                filePattern: "checkout/TestResults/PiPictureFrame.Tests/*.xml",
                                abortOnFail: true
                            );
                        }
                    }
                }
                stage( "Publish ARM32" )
                {
                    steps
                    {
                        Publish( "arm32_deb_pack" );
                    }
                }
                stage( "Publish ARM64" )
                {
                    steps
                    {
                        Publish( "arm64_deb_pack" );
                    }
                }
                stage( "Publish x64" )
                {
                    steps
                    {
                        Publish( "x64_deb_pack" );
                    }
                }
                stage( "Archive" )
                {
                    steps
                    {
                        archiveArtifacts "checkout/dist/*/bin/*.deb";
                    }
                }
            }
        }
    }
}
