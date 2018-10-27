import json
from flask import Flask, render_template
app = Flask(__name__)

active_users = []
active_users.append({
  'name': 'test_user',
  'games': ['game_one_dict', 'game_two_dict'],
  'email': 'test@test.com'
})

@app.route('/')
def index():
  return render_template('index.html')

@app.route('/<string:page_name>/')
def render_static(page_name):
    return render_template('%s.html' % page_name)

@app.route('/games/running/count')
def getNumberOfRunningGames():
  gameCount = 0
  for user in active_users:
    gameCount += len(user['games'])
  return str(gameCount)

if __name__ == '__main__':
  app.run(debug=True)