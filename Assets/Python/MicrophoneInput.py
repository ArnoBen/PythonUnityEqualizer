import pyaudio
import numpy as np
import matplotlib.pyplot as plt
import UDPSender

FORMAT = pyaudio.paInt16 # We use 16bit format per sample
CHANNELS = 1
RATE = 44100 #Equivalent of Fs
REFRESH_RATE = 20
CHUNK = int(RATE/REFRESH_RATE) # Comment wrong but written before : 1024bytes of data read from a buffer
#RECORD_SECONDS = 0.1

i=0
f,ax = plt.subplots(2)

# Prepare the Plotting Environment with random starting values
x = np.arange(CHUNK)
y = np.random.randn(CHUNK)

# Plot 0 is for raw audio data
li, = ax[0].plot(x, y)
ax[0].set_xlim(0,CHUNK)
ax[0].set_ylim(-6000,6000)
ax[0].set_title("Raw Audio Signal")
# Plot 1 is for the FFT of the audio
li2, = ax[1].plot(x, y)
ax[1].set_xlim(0,CHUNK/2)
ax[1].set_ylim(0,70)
ax[1].set_title("Fast Fourier Transform")
ax[1].set_xticks(np.arange(0,int(CHUNK/2),100))
ax[1].set_xticklabels(ax[1].get_xticks()*int(REFRESH_RATE))

# Show the plot, but without blocking updates
plt.pause(0.01)
plt.tight_layout()

audio = pyaudio.PyAudio()

# start Recording
stream = audio.open(format=FORMAT,
                    channels=CHANNELS,
                    rate=RATE,
                    input=True)#,
                    #frames_per_buffer=CHUNK)

global keep_going
keep_going = True

def computeFFT(data):
    return 10.*np.log10(abs(np.fft.rfft(data)))

def plot_data(in_data):
    # get and convert the data to float
    audio_data = np.frombuffer(in_data, np.int16)
    # Fast Fourier Transform, 10*log10(abs) is to scale it to dB
    # and make sure it's not imaginary
    
    dfft = computeFFT(audio_data)
    #print((np.floor(dfft[0:128]*10)/10).tolist())
    udpsender.send((np.floor(dfft*10)/10).tolist())
    # Force the new data into the plot, but without redrawing axes.
    # If uses plt.draw(), axes are re-drawn every time

    li.set_xdata(np.arange(len(audio_data)))
    li.set_ydata(audio_data)
    li2.set_xdata(np.arange(len(dfft)))
    li2.set_ydata(dfft)
    

    # Show the updated plot, but without blocking
    plt.pause(1/REFRESH_RATE)
    if keep_going:
        return True
    else:
        return False

# Open the connection and start streaming the data
stream.start_stream()
print("\n+---------------------------------+")
print("| Press Ctrl+C to Break Recording |")
print("+---------------------------------+\n")

# Loop so program doesn't end while the stream callback's
# itself for new data

udpsender = UDPSender.Sender()
while keep_going:
    try:
        data = stream.read(CHUNK)
        plot_data(data)
        #udpsender.send(computeFFT(data)[1:10].tolist())
        #udpsender.send(computeFFT(data)[0:128])
    except KeyboardInterrupt:
        keep_going=False
    except:
        pass

# Close up shop (currently not used because KeyboardInterrupt
# is the only way to close)
stream.stop_stream()
stream.close()

audio.terminate()