node {
  stage('SCM') {
    checkout scm
  }
  stage('SonarQube Analysis') {
    def msbuildHome = tool 'Default MSBuild'
    def scannerHome = tool 'SonarScanner for MSBuild'
    withSonarQubeEnv() {
      cd src
      bat "\"${scannerHome}\\SonarScanner.MSBuild.exe\" begin /k:\"ally-macgregor-sonarsource_geo-location_AYvs65M5fVPk5wpdf_It\""
      bat "\"${msbuildHome}\\MSBuild.exe\" /t:Rebuild"
      bat "\"${scannerHome}\\SonarScanner.MSBuild.exe\" end"
    }
  }
}

