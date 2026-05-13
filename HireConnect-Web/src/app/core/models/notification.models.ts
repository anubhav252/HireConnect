export interface Notification {
  notificationId: number;
  userId: number;
  message: string;
  type: string; // 'Info', 'Success', 'Warning', 'Error'
  isRead: boolean;
  createdAt: string;
}
