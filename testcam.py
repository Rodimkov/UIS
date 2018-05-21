# -*- coding: utf-8 -*-
"""
Created on Tue May 15 11:55:55 2018

@author: Юра
"""


import cv2
import pickle
import os
import face_recognition


def load_svm(name):
    predictor = cv2.ml.SVM_load(name + 'svm.xml')
    return predictor

def load_vocabulary(name):
    with open(name + 'vocabulary' , 'rb') as f:
        return pickle.load(f) 

def get_img_paths(data_path):
    im_paths = []
    paths = os.listdir(data_path)
    for i in paths:
        im_paths.append(data_path + '/' + i)
    return im_paths

def resize_and_detect(im_paths):
    cascadePath = "haarcascade_frontalface_default.xml"
    faceCascade = cv2.CascadeClassifier(cascadePath)
    #res = []
    for i in range (len(im_paths)):
        img = cv2.imread(im_paths[i],0)
        faces = faceCascade.detectMultiScale(img, 1.2,5)
        for(x,y,w,h) in faces:
            small_gray = img[y:y+h,x:x+w] 
            small_gray = cv2.resize(small_gray,(256,256))
            #res.append(small_gray)    
    return small_gray
            
def feature_extract(gray, bow_extr,detector):
    #gray = cv2.imread(path)
    kp = detector.detect(gray,None)
    return bow_extr.compute(gray, kp)    


flag = True
test = 'photo'

datasave = "resultmodel"
dataphoto = "res"

im = get_img_paths(test)
img = resize_and_detect(im)



you = face_recognition.load_image_file("you.jpg")
you_encoding = face_recognition.face_encodings(you)[0]
    
known_face_encodings = [
        you_encoding
]


detector = cv2.xfeatures2d.SURF_create()  
bow_extract = cv2.BOWImgDescriptorExtractor(detector,cv2.BFMatcher(cv2.NORM_L2))
paths = os.listdir(datasave)

result = 0

vocabulary = []
predictor = []

for i in paths:
    names = os.listdir(datasave+"/"+i)
    vocabulary.append(load_vocabulary(datasave+"/" + i + '/')) 
    predictor.append(load_svm(datasave+"/" + i + '/'))
  
    
while(flag):
    
    while(True):
        my_file = open("communication.txt", 'r')
        my_string = my_file.read()
        my_file.close()
        if(my_string == 'start'):
            break
        if(my_string == 'close'):
            flag = False
            break
        
    if(flag):
    
        imgrec = cv2.imread("photo/you.jpg")
        small_frame = cv2.resize(imgrec, (0, 0), fx=0.25, fy=0.25) # надо ли и как именно
        rgb_small_frame = small_frame[:, :, ::-1]
        
        face_locations = face_recognition.face_locations(rgb_small_frame)
        
        face_encodings = face_recognition.face_encodings(rgb_small_frame, face_locations)
        
        for face_encoding in face_encodings:
            matches = face_recognition.compare_faces(known_face_encodings, face_encoding)
            result = 2
            if True in matches:
                result = 1
            
        for i in range(len(paths)):          
            bow_extract.setVocabulary(vocabulary[i]) # хм а он очищается?
            _,r = predictor[i].predict(feature_extract(img,bow_extract,detector))
            result += r[0]
            
            
        print(result/3)