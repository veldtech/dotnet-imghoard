<p align="center">
  <img src="https://cdn.miki.ai/nuget/miki-logo@0.5x.png"/>
</p>
<p align="center">
    Mikibot/imghoard API implementation in C#
</p>

<p align="center">
  <!-- Nuget package -->
  <a href="https://www.nuget.org/packages/{NUGET_PACKAGE}">
      <img src="https://img.shields.io/nuget/dt/Miki.Discord.svg"/>
  </a>

  <!-- Discord dev server -->
  <a href="https://discord.gg/XpG4kwE">
    <img src="https://img.shields.io/discord/259343729586864139.svg?logo=discord"/>
  </a>
  <!-- Add additional badged here -->
</p>

<!-- Build status of MASTER only -->

# .Net Imghoard

## Project status

|Environment | Platform | PlatformN|
|---|---|---|
|env_debug | <img src="https://dev.azure.com/mikibot/Miki/_apis/build/status/{REPO_NAME}?branchName={BRANCH}"/> | <img src="https://dev.azure.com/mikibot/Miki/_apis/build/status/{REPO_NAME}?branchName={BRANCH}"/>|
| env_release | <img src="https://dev.azure.com/mikibot/Miki/_apis/build/status/{REPO_NAME}?branchName={BRANCH}"/> | <img src="https://dev.azure.com/mikibot/Miki/_apis/build/status/{REPO_NAME}?branchName={BRANCH}"/>|

## Getting started

This is where you explain some basic logic about your project.

```cs
//Use Default Settings
ImghoardClient client = new ImghoardClient();
//Pass String Or Uri into constructor
ImghoardClient client = new ImghoardClient("https://imghoard.example.com");

//Get All Images Without Tags
var images = await client.GetImagesAsync();
//Get All Images Of Specfic Tag
var images = await client.GetImagesAsync("animal");
//Get All Images Of Specific Tag Excluding Tag
var images = await client.GetImagesAsync("animal", "-cat");
//Get A specific image by it's Id
var image = await client.GetImageAsync(1169529585188999168);

//Posts A new Image
var imageurl = await client.PostImage(imageStream, "supercool", "image", "with", "amazing", "tags");
```

and generally explaining how to get around it.
