# pic-hub
Picture viewing and management platform. 

Simple, right? Well, maybe not so simple. So I've created a getting started site here > [Getting Started](https://github.com/paananen/pic-hub/wiki)

Remember this is a work in progress - so things might be a bit jankey for a while.

...but...

If you've got an idea or see something you can help with? Go for it! I don't have a contribution guide yet (because... see [ToDo](https://github.com/paananen/pic-hub/blob/master/README.md#todo-list) list), so feel free to write some code and ~~send it in~~ 

`git add . `

`git commit -m "Look I added some code for you"`

`git push  <REMOTENAME> <BRANCHNAME> `

...or something like that.

**Issues** are also an option. They're very easy to add and keep track of.
😄

## The Issue
![Photo Properties](https://github.com/paananen/pic-hub/raw/master/screencaps/photo-properties.png)

30,000+ photos on a NAS and no good way to view them.

## The Idea
A website to view and manage the photos. We want to be able to do the lease amount of work possible to get photos of our cameras and phones and make them viewable on this site.

## ToDo List
- [x] Create read me
- [ ] Finish read me (will it ever really be "finished"?)
- [ ] Create wiki
- [ ] Create a contribution guide (I feel like thats a thing I've seen on other projects)
- [ ] Google What a contribution guide is and what it should contain
- [ ] Add the first component of the project and start writing some code!

## Current Stuff
The current solution I'm running has four parts. They currently work (mostly) but I've not create a public GitHub :octocat: project before and wanted to try this out. So I'm starting again and will be pushing updates here. 

1. **Photo Scanner** A console application that scans the photos directory and writes all the photo data to an SQL Server database.
    * - [x] Create solution
    * - [x] Add to git project
2. **Photo API** An API that serves information from the photos database (because I wanted my very own an API).
    * - [x] Create solution
    * - [ ] Add to git project
3. **Photo FTP** An FTP site hosting the photos.
    * - [ ] Is an FTP site the best way to do this?
    * - [ ] Create a wiki article on this
4. **PicHub** A website to view the photos.
    * - [ ] Create solution
    * - [ ] Add to git project

**Disclaimer**: *The final solution doesn't need to follow this same path. I've just done this in seperate parts because I like breaking thinks down into small components and working on things piece by piece.*

### Screenshots
*This is an example that I currently have running at home. I'll (hopefully) be cleaning things up a little as I try replicate what I've done so far.*

<img src="https://github.com/paananen/pic-hub/raw/master/screencaps/pichub-default-view.png" width= "205" height="365">&nbsp;<img src="https://github.com/paananen/pic-hub/raw/master/screencaps/pichub-grid-view.png" width= "205" height="365">&nbsp;<img src="https://github.com/paananen/pic-hub/raw/master/screencaps/pichub-folder-view.png" width= "205" height="365">&nbsp;<img src="https://github.com/paananen/pic-hub/raw/master/screencaps/pichub-default-view-with-menu.png" width= "205" height="365">

## Future Ideas
* Photo tags (manual and maybe auto if possible)
* Photo view counters
  * This could also be used as a weight when getting random photos
    * i.e. if view number is large then there should be less change of it being displayed in random photos
* Photo stats
  * Camera
  * Lens
  * Time of day
  * ISO
  * Shutter speet
  * Aperture
  * Etc...
* Search functionality
* Private photos (that need an approved login to see)
* Face recognition and tagging
* A settings page
* Add photos from multiple local sources
* Add Google photo
  * [Google Drive API > Rest > Files: list](https://developers.google.com/drive/v3/reference/files/list)
* User login(s) - admins, viewers
* Add comments to photos
* Ability to use other database platforms (mongo, postgress, etc.)
* node.js version of the viewing site (because I'd like to learn how)
* Videos! People make videos sometimes too right.

# With Help From...
* [drewnoakes/metadata-extractor-images](https://github.com/drewnoakes/metadata-extractor-images)
* [Newtonsoft](https://www.newtonsoft.com/json)
