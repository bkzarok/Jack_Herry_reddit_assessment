1. Use the appsettings.json file, which should be provided to you. 
2. Place it in the same directory as the program executable.
   
The appsetting .net json file  has all the credential provided. 
Altough this is not usually the way to store these credential I think
it will make it alot easier for who is evaluating this project.

Example appsettings.json:

```
{
  "AppSettings": {
    "clientid": "",
    "secrets": "",
    "username": "",
    "password": "",
    "grant_type": "",
    "tokenUrl": "https://www.reddit.com/api/v1/access_token",
    "subredditurls": [
      "https://oauth.reddit.com/r/askreddit/new",
      "https://oauth.reddit.com/r/gaming/new",
      "https://oauth.reddit.com/r/python/new"
    ],
    "user_agent":  "subredditapi/v1"
  }
}

```
