import requests
import json

# def getInfo(steamid):
#  addr = 'https://store.steampowered.com/api/appdetails?appids=' + str(steamid) + '&l=en'
#  res = requests.get(addr)
#  return res.json()

# with open('debug.json', 'w') as f:
#   json.dump(getInfo(730), f)

def getGameInfo(steamid):
  addr = 'https://store.steampowered.com/api/appdetails?appids=' + str(steamid) + '&l=en'
  res = requests.get(addr)
  _, content = res.json().popitem()
  if content['success']:
    data = content['data']
    d = {
      'Title': data['name'],
      'Link': "https://store.steampowered.com/app/" + str(steamid),
      'ImgUrl': data['header_image'],
      'Description': data['short_description'],
      'IsFree': data['is_free']
    }
    return d
  else:
    d = {
      'Title': 'Uknown',
      'Link': '#',
      'ImgUrl': '#',
      'Description': 'No description',
      'IsFree': False
    }
    return d

def getGameInfoJson(steamid):
  return json.dumps(getGameInfo(steamid))

def getGames():
  with open('LanServer/static/games.json', 'rb') as f:
    data = json.load(f)
  
  for game in data['Games']:

    if 'SteamID' in game:
      info = getGameInfo(game['SteamID'])
      game.update(info)
      if info['IsFree'] == True:
        game['Cost'] = '0 EUR'
  return json.dumps(data)

getGames()