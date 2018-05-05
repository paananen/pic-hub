# pic-hub
Picture viewing and management platform.

## The Issue
![Photo Properties](https://github.com/paananen/pic-hub/raw/master/screencaps/photo-properties.png)

30,000+ photos on a NAS and no good way to view them.

## The Idea
A website to view and manage the photos.

## Current Stuff
The current solution has four parts.
1. A console application that scans the photos directory and writes all the photo data to an SQL Server database.
2. An API that serves information from the photos database (because I wanted my very own an API).
3. An FTP site hosting the photos.
4. A website to view the photos.

### Screenshots
<img src="https://github.com/paananen/pic-hub/raw/master/screencaps/pichub-default-view.png" width= "205" height="365">&nbsp;<img src="https://github.com/paananen/pic-hub/raw/master/screencaps/pichub-grid-view.png" width= "205" height="365">&nbsp;<img src="https://github.com/paananen/pic-hub/raw/master/screencaps/pichub-folder-view.png" width= "205" height="365">&nbsp;<img src="https://github.com/paananen/pic-hub/raw/master/screencaps/pichub-default-view-with-menu.png" width= "205" height="365">

## Future Ideas
* Photo tags (manual and maybe auto if possible)
* Search functionality
* Face recognition and tagging
* A settings page
* Add photos from multiple local sources
* Add Google photo
* User login(s) - admins, viewers
* Add comments to photos
* Ability to use other database platforms (mongo, postgress, etc.)
* node.js version of the viewing site (because I'd like to learn how)
