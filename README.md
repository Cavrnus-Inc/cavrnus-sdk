
<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/cav-logo.png" alt="Cavrnus Banner"/>
</p>

# <p style="text-align: center;">Cavrnus Spatial Connector</p>

<h4 align="center">
  <a href="https://www.cavrnus.com/">
    <img src="https://img.shields.io/badge/Cavrnus%20Website-label?style=flat&color=white&link=https%3A%2F%2Fwww.cavrnus.com%2F
    " alt="cavrnus" style="height: 20px">
  </a>
    <a href="https://www.youtube.com/@cavrnus">
    <img src="https://img.shields.io/badge/Cavrnus%20YouTube-label?style=flat&logo=YouTube&logoColor=red&labelColor=white&color=white&link=https%3A%2F%2Fwww.youtube.com%2F%40cavrnus
    " alt="youtube"style="height:20px">
  </a>
  <a href="https://twitter.com/cavrnus">
    <img src="https://img.shields.io/badge/Cavrnus_Twitter-label?style=flat&logo=x&logoColor=black&labelColor=white&color=white&link=https%3A%2F%2Fwww.youtube.com%2F%40cavrnus
    " alt="twitter" style="height: 20px;"></a>
    <a href="https://discord.gg/AzgenDT7Ez">
    <img src="https://img.shields.io/badge/Cavrnus_Support-label?style=flat&logo=discord&labelColor=white&color=white&link=https%3A%2F%2Fdiscord.gg%2FAzgenDT7Ez
    " alt="discord" style="height: 20px;"></a>
</h4>

<h4 align="center">
    <img src="https://img.shields.io/badge/Version-2.13.2-label?style=flat&labelColor=blue&color=white&link=https%3A%2F%2Fwww.cavrnus.com%2F
    " alt="discord" style="height: 20px;"></a>
    </h4>

    
## Package Information  
**Version:** 2.13.2    
**Last Updated:** 2024-09-30    
**Documentation:** [Cavrnus Spatial Connector Unity Documentation](https://cavrnus.atlassian.net/wiki/spaces/CSM/overview?homepageId=410615958)     


## Introduction

### Overview
Cavrnus provides tools to add multi-user collaboration, real-time communication, and an editable 3D environment in a persistent virtual space. The tool enhances 3D workflows by offering a seamless drag-and-drop toolkit for embedding collaborative features directly into Unity projects.

### Requirements
- Unity 2020 or higher. Cavrnus is mostly agnostic to Unity versions.
- All samples are created in URP. Functionilty will not be affected if using other rendering pipelines, however, shaders and materials may need to be converted.
- For best results when creating Unity builds, refer to [Required Project Settings](https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/845381657/Required+Project+Settings) documentation.

## Getting Started

### Installation
For all methods of installing Cavrnus to your project, see [Add Cavrnus To Your Project](https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/827916347/Add+Cavrnus+to+Your+Project) documentation.

We recommend using the following Package Registry installation.
### Scoped Registry Deployment

1. Open Project Settings
     
<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/scoped-registry-package-manager-open-ps.png" alt="Cavrnus open project settings"/>
</p>

2. Navigate to Package Manager and add a new scoped registry using the following information:
    * a. <b>Name</b>: CSC (Any name is applicable here)
    * b. <b>URL</b>: https://packages.cavrn.us
    * b. <b>Scope(s)</b>: com.cavrnus

<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/scoped-registry-package-manager.png" alt="Cavrnus open scoped registry window"/>
</p>

3. Open the Package Manager window.

<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/scoped-registry-package-manager-open-pm.png" alt="Cavrnus open my registries"/>
</p>

4. Once open, navigate to <b>My Registries</b>.

<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/scoped-registry-package-manager-my-reg.png" alt="Cavrnus open my registries"/>
</p>

5. Press <b>Install</b> to the load the package.

<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/scoped-registry-package-manager-install.png" alt="Cavrnus install"/>
</p>

6. Cavrnus Spatial Connector is now ready to be used!

### Setup
1. Once the Cavrnus Spatial Connector package is installed, import the sample scene.

<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/open-sample-package-manager.png" alt="Cavrnus import sample scene"/>
</p>

2. In the Project view, search for <b>Sample Cavrnus Connected Space</b> located in the newly added <b>Samples/Cavrnus Spatial Connector</b> folder.
3. Press play to enter Play Mode! Notice you will join a space with many example components to modify. Ajdust a few and end play mode.
4. Toggle play mode on. Upon reentering the space, notice the changes are persistent!

### Quick Start Guide
Starting from an existing scene is easy!

1. Navigate to and select <b>Tools>Cavrnus>Setup Scene For Cavrnus</b>.
2. Your scene now has the necessary Cavrnus components.
2. Edit the newly added <b>Cavrnus Spatial Connector</b> scene prefab to adjust to your project's requirements.

For in-depth help with setting up the Cavrnus Spatial Connector prefab, see [Setup Your Scene](https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/827916295/Setup+Your+Scene) documentation.

## Feature Guide

### **Multi-User Copresence** 
Copresense works right out of the box. It's as easy as following the [Setup Guide](https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/827916295/Setup+Your+Scene) and ensuring your scene has the <b>Cavrnus Spatial Connector</b> prefab. When another client joins the space, remote Cavrnus-provided avatars will appear and represent others in the space.

### **No Code Components** 
Everything inside the Cavrnus Plugin is built on top of the functions exposed in Cavrnus For Developers and our API Reference. However, those may be unneeded for most applications. Instead, for basic use-cases, we provide helpful components that let you immediately start to synchronize your projects without any custom dev work.

Please refer to [No-Code Collaboration Documentation](https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/895254561/Cavrnus+No-Code+Collaboration+Unity) for more information!

For a quick example, lets [sync a capsule's transform](https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/828178434/Sync+Property+Components+Unity).

<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/sync-transform-add-component.png" alt="Cavrnus search for SyncLocalTransform component"/>
</p>

If you type “Sync” you should see autocomplete options for the Property Synchronizers we provide. For this example let’s select Sync Local Transform.

<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/sync-transform-adding.png" alt="Cavrnus adding SyncLocalTransform component"/>
</p>

Adding this component does two things. It adds a Sync Transform component, and it adds a Cavrnus Properties Container if one is not already present.

<p align="center">
    <img src="https://raw.githubusercontent.com/Cavrnus-Inc/cavrnus-spatial-connector-unity/master/Resources%7E/Readme/sync-transform-result.png" alt="Cavrnus adding SyncLocalTransform component"/>
</p>

It will auto-fill a Property Name based on the type of the component, but this can be changed to whatever string you want. The Cavrnus Properties Container will also automatically fill in a Unique Container Name based on the object’s hierarchy.  

And that's it! This capsule's transform data is now be synced!

### **Full API Reference** 
The Cavrnus Spatial Connector also includes an [API reference](https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/824934449/API+Reference+Unity) upon which the No-Code components are built from. This provides the needed flexibility in use cases where No-Code components may not be applicable.

## Support and Feedback
Do you need help? Have you found a bug? Reach out through the [Cavrnus Discord](https://discord.gg/AzgenDT7Ez).