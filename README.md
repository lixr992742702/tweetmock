# Project4.2 : Twitter Simulator

**Group members**

1. Zhaochen Jiang, 7991-0054, zhaochen.jiang@ufl.edu
2. Xiangrui Li, 1189-3317, xiangrui.li@ufl.edu

# Project Definition

-  implement a WebSocket interface to our part I implementation

# Execution Steps
 
- Open and run /project4_2/WebSocket.sln to activate server
- Run Twitter Clone.html to open UIs.
- How to use:
	## Register 	
	Input username and password connected with "*", for example: enter "user1*1111", then click Register button. If succeed, return "Register succeed".
	## Login  		
	Similar with Register, input username and password connected with "*", then click Login button. If succeed, return "Login succeed, username, password".
	## Logout  	
	Click logout button to disconnect account. If succeed, return "Logout succeed".
	## Subscribe  	
	Input username you want to follow and click Subscribe button.If succeed, reutrn "Subscribe succeed, follow tweet from username".
	## Tweet  		
	Input tweet as "content#hashtag@username", for example: enter "I love cats!#cats@user2", click Tweet button to tweet. If succeed, return "Tweet Succeed!" and tweet content, hashtag and mention.
	## Retweet  	
	When you receive a live tweet, the tweet id will be shown on screen. If you want to retweet, enter this tweet id and click retweet button. If succeed, return "Retweet:" with tweet content, hashtag and mention. 
	## Query hashtag 
	Input a hashtag, then click HashtagSearch button to get all tweets with this hashtag.
	## Query subscribed 
	Click SubscribedTweets button to get all tweets you subscribed.
	## Query Mention 
	Click Mymention button to get all tweets mention you.


# Code explain
> In this project, we implementation of WebSockets using Suave. For client, it can send message to server 
through websocket, and recieve message from server. For Server, it can listen the client, read and send message. 


